#pragma once
#include "Record.h"


class Database
{
public:
	Database();
	Database(Database&&);
	~Database();
	Database& operator=(Database&&);

	Database(const Database&) = delete;
	Database& operator=(Database&) = delete;

	using rank_t = Record<std::string, int>;

	/*
	get range for iterator. you can use such style:

	0th elem: name
	1st elem: score
	*/
	rank_t get_ranks(size_t size);
	bool put_rank(const std::string& name, int score);

private:
	template<class ...Ts>
	auto _get_record_range(std::string sql)
	{
		return Record<Ts...>{m_db, sql};
	}

	void _initialize_table();

private:
	sqlite3* m_db{};
};