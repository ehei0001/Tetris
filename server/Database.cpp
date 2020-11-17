#include "stdafx.h"
#include <cassert>
#include <functional>

#include "Database.h"


Database::Database()
{
	auto result = ::sqlite3_open("tetris.db", &m_db);

	if (SQLITE_OK == result) {
		_initialize_table();
	}
	else {
		throw std::string("db open failed");
	}
}

Database::Database(Database&& database)
{
	this->m_db = database.m_db;
	database.m_db = nullptr;
}

Database& Database::operator=(Database&& database)
{
	this->m_db = database.m_db;
	database.m_db = nullptr;

	return *this;
}

Database::~Database()
{
	if (m_db) {
		auto result = ::sqlite3_close(m_db);

		assert(SQLITE_OK == result);
	}
}

Database::Rank_type Database::get_ranks(size_t size)
{
	constexpr auto sql = "SELECT NAME, SCORE FROM RANK ORDER BY SCORE";

	return _get_record_range<std::string, int>(sql);
}

bool Database::put_rank(const std::string& name, int score)
{
	std::ostringstream stream;
	stream << "INSERT INTO RANK (NAME, SCORE) VALUES ('" << name << "', " << score << ");";

	char* message_error{};
	auto result = sqlite3_exec(m_db, stream.str().c_str(), {}, {}, &message_error);

	if (result == SQLITE_OK) {
		return true;
	}
	else {
		std::string message_error_{ message_error };
		::sqlite3_free(message_error);

		std::cerr << message_error << std::endl;

		return {};
	}
}

void Database::_initialize_table()
{
	constexpr auto table_name = "RANK";
	std::ostringstream stream;
	stream << "SELECT name FROM sqlite_master WHERE type = 'table' AND name = '" << table_name << "';";

	auto records = _get_record_range<std::string>(stream.str());
	auto record_it = std::begin(records);
	auto record = *record_it;

	if (record.empty()) {
		constexpr auto sql = \
			"CREATE TABLE RANK( \
					ID INTEGER PRIMARY KEY AUTOINCREMENT, \
					NAME TEXT NOT NULL, \
					SCORE INT NOT NULL \
				)";
		char* message_error{};
		auto result = ::sqlite3_exec(m_db, sql, {}, {}, &message_error);

		if (result == SQLITE_OK) {
			std::cout << "table created: " << table_name << std::endl;
		}
		else {
			std::string message_error_{ message_error };
			::sqlite3_free(message_error);

			throw message_error_;
		}
	}
	else {
		assert(std::get<0>(record[0]) == table_name);
	}
}