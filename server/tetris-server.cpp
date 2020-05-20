// tetris-server.cpp : 이 파일에는 'main' 함수가 포함됩니다. 거기서 프로그램 실행이 시작되고 종료됩니다.
//

#include "stdafx.h"
#include "Database.h"

using namespace web;
using namespace utility;
using namespace http;
using namespace web::http::experimental::listener;

#define NAME U("name")
#define SCORE U("score")


namespace tetris {
	class Tetris
	{
		uri_builder m_uri;
		http_listener m_listener_get_rank;
		http_listener m_listener_put_rank;
		DataBase m_dataBase;

	public:
		Tetris( web::uri_builder& uri ) :
			m_uri{ uri },
			m_listener_get_rank{ uri_builder{uri}.append_path(U("get_rank")).to_uri() },
			m_listener_put_rank{ uri_builder{uri}.append_path(U("put_rank")).to_uri() }
		{
			m_listener_get_rank.support( methods::GET, [=]( http_request message ) { handle_get_rank( message ); } );
			m_listener_put_rank.support( methods::GET, [=]( http_request message ) { handle_put_rank( message ); } );
		}

		~Tetris()
		{
			for (auto l : { &m_listener_get_rank, &m_listener_put_rank }) {
				l->close().wait();
			}
		}

		void wait() 
		{
			for (auto l : { &m_listener_get_rank, &m_listener_put_rank }) {
				ucout << utility::string_t(U("Listening for requests at: ")) << l->uri().to_string() << std::endl;

				auto task = l->open();
				task.wait();
			}
		}

	private:
		void handle_get_rank( http_request message )
		{
			ucout << message.to_string() << std::endl;

			constexpr size_t rank_count{ 10 };

			auto rank_range = m_dataBase.get_ranks(rank_count);
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

			if (array_index == array_index_init_value) {
				message.reply(status_codes::NotFound);
			}
			else {
				message.reply(status_codes::OK, rank_array);
			}
		}

		void handle_put_rank( http_request message )
		{
			ucout << message.to_string() << std::endl;

			std::string name{ "name" };
			int score{ 99 };

			if (m_dataBase.put_rank(name, score)) {
				message.reply(status_codes::OK);

				std::cout << "put rank:" << name << ", " << score << std::endl;
			}
			else {
				message.reply(status_codes::NotAcceptable);

				std::cerr << "failed put rank" << std::endl;
			}
		}
	};
}


#ifdef _WIN32
int wmain( int argc, wchar_t* argv[] )
#else
int main( int argc, char* argv[] )
#endif
{
	utility::string_t port = U( "34568" );
	if ( argc == 2 )
	{
		port = argv[1];
	}

	utility::string_t address = U( "http://localhost:" );
	address.append( port );

	uri_builder uri(address);
	uri.append_path(U("tetris"));

	tetris::Tetris tetris{ uri };
	tetris.wait();

	std::cout << "Press ENTER to exit." << std::endl;

	std::string line;
	std::getline( std::cin, line );

	return {};
}

// 프로그램 실행: <Ctrl+F5> 또는 [디버그] > [디버깅하지 않고 시작] 메뉴
// 프로그램 디버그: <F5> 키 또는 [디버그] > [디버깅 시작] 메뉴

// 시작을 위한 팁: 
//   1. [솔루션 탐색기] 창을 사용하여 파일을 추가/관리합니다.
//   2. [팀 탐색기] 창을 사용하여 소스 제어에 연결합니다.
//   3. [출력] 창을 사용하여 빌드 출력 및 기타 메시지를 확인합니다.
//   4. [오류 목록] 창을 사용하여 오류를 봅니다.
//   5. [프로젝트] > [새 항목 추가]로 이동하여 새 코드 파일을 만들거나, [프로젝트] > [기존 항목 추가]로 이동하여 기존 코드 파일을 프로젝트에 추가합니다.
//   6. 나중에 이 프로젝트를 다시 열려면 [파일] > [열기] > [프로젝트]로 이동하고 .sln 파일을 선택합니다.

