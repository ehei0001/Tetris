#include "stdafx.h"
#include <codecvt>
#include <execution>
#include <locale>
#include <sstream>

#include "Database.h"
#include "Server.h"


Server::Server(web::uri_builder& uri) :
	m_uri{ uri },
	m_listener_get_rank{ uri_builder{uri}.append_path(U("get_rank")).to_uri() },
	m_listener_put_rank{ uri_builder{uri}.append_path(U("put_rank")).to_uri() },
	m_db{ std::make_unique<Database>() }
{
	m_listener_get_rank.support(methods::GET, [=](http_request message) { handle_get_rank(message); });
	m_listener_put_rank.support(methods::GET, [=](http_request message) { handle_put_rank(message); });
}

void Server::wait()
{
	auto listener_job = [](auto listener) {
		ucout << utility::string_t(U("Listening for requests at: ")) << listener->uri().to_string() << std::endl;

		auto task = listener->open();
		task.wait();
	};
	auto listeners = { &m_listener_get_rank, &m_listener_put_rank };

	std::for_each(std::begin(listeners), std::end(listeners), listener_job);
}

void Server::handle_get_rank(http_request message)
{
	ucout << message.to_string() << std::endl;

	constexpr size_t rank_count{ 10 };
	
	auto ranks = m_db->get_ranks(rank_count);
	auto rank_array = json::value::array(rank_count);
	auto update_array = [array = &rank_array](auto column) {
		auto row = column.first;
		auto& result = column.second;
		auto name = std::get<0>( result[0] );
		utility::string_t name_{ std::cbegin( name ), std::cend( name ) };

		constexpr auto name_key = U( "name" );
		constexpr auto score_key = U( "score" );

		auto score = std::get<1>( result[1] );
		auto values = json::value::object();
		values[name_key] = json::value::string( name_ );
		values[score_key] = json::value::number( score );
		array[row] = values;
	};

	//std::for_each(std::execution::seq, std::begin( ranks ), std::end( ranks ), update_array );
	std::for_each( std::begin( ranks ), std::end( ranks ), update_array );

	message.reply(status_codes::OK, rank_array);
}

void Server::handle_put_rank(http_request message)
{
	auto split_query = uri::split_query( message.request_uri().query() );
	auto name_iter = split_query.find( U( "name" ) );
	auto score_iter = split_query.find( U( "score" ) );

	if (name_iter == split_query.end() || score_iter == split_query.end()) {
		message.reply( status_codes::NotAcceptable );

		std::cerr << "uri is wrong: name and score" << std::endl;
	}
	else
	{
		// https://stackoverflow.com/questions/15287001/is-there-any-universal-way-to-convert-wstring-to-stdstring-considering-all-pos
		auto code_page = GetACP();
		auto name_pointer = name_iter->second.c_str();
		auto buffer_size = ::WideCharToMultiByte( code_page, 0, name_pointer, -1, 0, 0, 0, 0 );
		auto buffer = static_cast<char*>(alloca( buffer_size ));
		::WideCharToMultiByte( code_page, 0, name_pointer, -1, buffer, buffer_size, 0, 0 );

		std::wstringstream sstream( score_iter->second );
		int score{};
		sstream >> score;

		if (m_db->put_rank( buffer, score )) {
			message.reply( status_codes::OK );

			std::cout << "put rank:" << buffer << ", " << score << std::endl;
		}
	}
}