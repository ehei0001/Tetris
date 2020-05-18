// tetris-server.cpp : 이 파일에는 'main' 함수가 포함됩니다. 거기서 프로그램 실행이 시작되고 종료됩니다.
//

#include "stdafx.h"

using namespace web;
using namespace utility;
using namespace http;
using namespace web::http::experimental::listener;

namespace tetris {
	class IRecordRange
	{};


	struct IDataBase 
	{
	public:
		virtual IRecordRange get_rank( size_t size ) const = 0;
	};


	class DataBase : public IDataBase
	{
		sqlite3* m_db{};

	public:
		class RecordRange : public IRecordRange
		{
			sqlite3* m_db{};
			std::string m_sql;

		public:
			class Iterator
			{
				using Record = std::tuple<std::string, size_t>;
				sqlite3_stmt* m_statement{};
				int m_sqlite3_status{ SQLITE_DONE };

			public:
				Iterator( sqlite3* db, std::string sql )
				{
					auto result = sqlite3_prepare_v2( db, sql.c_str(), -1, &m_statement, {} );

					if ( result != SQLITE_OK ) {
						auto message = sqlite3_errmsg( db );

						throw std::string( message );
					}
				}

				Iterator( int status ) : m_sqlite3_status{ status }
				{}

				~Iterator()
				{
					sqlite3_finalize( m_statement );
				}

				Iterator& operator++()
				{
					m_sqlite3_status = sqlite3_step( m_statement );

					return *this;
				}

				Record operator*()
				{
					auto name{ sqlite3_column_text( m_statement, 0 ) };
					auto score{ sqlite3_column_int( m_statement, 1 ) };
					auto _name = std::string{ reinterpret_cast<const char*>( name ) };

					return { _name, score };
				}

				bool operator==( const Iterator& lhs ) const
				{
					return lhs.m_sqlite3_status == this->m_sqlite3_status;
				}

				bool operator!=( const Iterator& lhs ) const
				{
					return !operator==( lhs );
				}
			};

		public:
			RecordRange( sqlite3* db, std::string sql ) : m_db{ db }, m_sql{ sql }
			{}

			Iterator begin()
			{
				return { m_db, m_sql };
			}

			Iterator end()
			{
				return { static_cast<int>( SQLITE_DONE ) };
			}
		};

		DataBase()  
		{
			auto rc = sqlite3_open( "tetris.db", &m_db );

			if ( rc ) {
				auto sql = \
					"CREATE TABLE RANK( \
						ID INT PRIMARY KEY NOT NULL, \
						NAME TEXT NOT NULL, \
						SCORE INT NOT NULL \
					)";
				char* messageError{};
				sqlite3_exec( m_db, sql, {}, {}, &messageError );

				if ( exit != SQLITE_OK ) {
					sqlite3_free( messageError );
				}
			}
		}

		virtual ~DataBase()
		{
			sqlite3_close( m_db );
		}

		IRecordRange get_rank( size_t size ) const override final
		{
			auto sql = "SELECT * FROM RANK ORDERED BY SCORE";
			RecordRange r{ m_db, sql };

			return r;
		}
	};


	class Tetris
	{
		const utility::string_t m_address;
		http_listener m_listener;
		DataBase m_dataBase;

	public:
		Tetris( const utility::string_t& address ) : 
			m_address{ address },
			m_listener{ address }
		{
			web::uri_builder uri( address );
			uri.append_path( U( "blackjack/dealer" ) );

			auto addr = uri.to_uri().to_string();

			m_listener.support( methods::GET, [=]( http_request message ) { handle_get( message ); } );
			m_listener.support( methods::PUT, [=]( http_request message ) { handle_put( message ); } );
			m_listener.support( methods::POST, [=]( http_request message ) { handle_post( message ); } );
		}

		~Tetris()
		{
			m_listener.close().wait();
		}

		void wait() 
		{
			ucout << utility::string_t( U( "Listening for requests at: " ) ) << m_address << std::endl;

			auto task = m_listener.open();
			task.wait();
		}

	private:
		void handle_get( http_request message )
		{
		}

		void handle_put( http_request message )
		{
		}

		void handle_post( http_request message )
		{}
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

	tetris::Tetris tetris{ address };
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

