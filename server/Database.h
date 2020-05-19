#pragma once
#include <string>
#include <tuple>

#include "IDatabase.h"
#include "sqlite3.h"


class DataBase : public IDataBase
{
	sqlite3* m_db{};

public:
	template<class T, class ...Ts>
	class RecordRange : public IRecordRange
	{
		sqlite3* m_db{};
		std::string m_sql;

	public:
		template<class T, class ...Ts>
		class Iterator
		{
			using RecordType = std::tuple<T, Ts...>;
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

		private:
			template
			auto get()
		};

	public:
		RecordRange( sqlite3* db, std::string sql ) : m_db{ db }, m_sql{ sql }
		{}

		Iterator begin()
		{
			return <T, Ts...>{ m_db, m_sql };
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