#include "stdafx.h"
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
	constexpr auto NAME = U("name");
	constexpr auto SCORE = U("score");

	auto rank_range = m_db->get_ranks(rank_count);
	auto rank_array = json::value::array(rank_count);
	auto rank_end_it = std::end(rank_range);
	constexpr int array_index_init_value{ -1 };
	int array_index{ array_index_init_value };

	for (auto it = std::begin(rank_range); it != rank_end_it; ++it) {
		auto r = *it;
		auto name = std::get<0>(r[0]);
		utility::string_t name_{ std::cbegin(name), std::cend(name) };

		auto score = std::get<1>(r[1]);
		auto values = json::value::object();
		values[NAME] = json::value::string(name_);
		values[SCORE] = json::value::number(score);
		rank_array[++array_index] = values;
	}

	message.reply(status_codes::OK, rank_array);
}

void Server::handle_put_rank(http_request message)
{
	ucout << message.to_string() << std::endl;

	std::string name{ "name" };
	int score{ 99 };

	if (m_db->put_rank(name, score)) {
		message.reply(status_codes::OK);

		std::cout << "put rank:" << name << ", " << score << std::endl;
	}
	else {
		message.reply(status_codes::NotAcceptable);

		std::cerr << "failed put rank" << std::endl;
	}
}