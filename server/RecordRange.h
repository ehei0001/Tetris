#pragma once
#include <cassert>
#include <variant>
#include <string>
#include <vector>

#include <sqlite3.h>


template<size_t COLUMN_COUNT, class T, class... Ts>
class Iterator
{
	using ValueType = std::variant<T, Ts...>;
	using RecordType = std::vector<ValueType>;

	sqlite3_stmt* m_statement{};
	int m_status{ SQLITE_DONE };

	const size_t m_hash{};

public:
	explicit Iterator(std::string sql, sqlite3* db, size_t hash) : m_status{ ::sqlite3_prepare_v2(db, sql.c_str(), -1, &m_statement, {}) }, m_hash{ hash }
	{
		assert(::sqlite3_column_count(m_statement) == COLUMN_COUNT);

		if (m_status == SQLITE_OK) {
			m_status = ::sqlite3_step(m_statement);
		}
		else {
			auto message = sqlite3_errmsg(db);

			throw std::string(message);
		}
	}

	explicit Iterator(std::string sql, size_t hash) : m_hash{ hash }, m_status{ SQLITE_DONE }
	{}

	~Iterator()
	{
		m_status = ::sqlite3_finalize(m_statement);
	}

	Iterator& operator++()
	{
		/*
		http://wiki.pchero21.com/wiki/Libsqlite3

		SQLITE_BUSY
		���� �����ͺ��̽��� lock �� �ɷ� ������ ���Ѵ�. �ٽ� ��õ��� �ϰų� �ѹ� ��, �ٽ� �õ��� �� �ִ�.
		SQLITE_DONE
		�ش� ���� ������ ���������� ����(����)�Ǿ����� �ǹ��Ѵ�. sqlite3_reset() ȣ�� ��, sqlite3_step()�� �����Ͽ��� �Ѵ�.
		SQLITE_ROW
		���� ����� �����Ͱ� ������ ��Ÿ����. select ������ ���� ��츦 �����غ� �� �ִµ�, ���� column_access �Լ����� ����� �� �ִ�.
		SQLITE_ERROR
		������ �߻������� ��Ÿ����.
		SQLITE_MISUSE
		�߸��� ���� ����.
		*/
		m_status = ::sqlite3_step(m_statement);

		return *this;
	}

	RecordType operator*()
	{
		if (m_status == SQLITE_ROW) {
			RecordType record;
			_get_columns<T, Ts...>(m_statement, record);

			return record;
		}
		else {
			assert(m_status == SQLITE_DONE);
			return {};
		}
	}

	bool operator==(const Iterator& lhs) const
	{
		assert(lhs.m_hash == m_hash);

		return lhs.m_status == m_status;
	}

	bool operator!=(const Iterator& lhs) const
	{
		return !operator==(lhs);
	}

private:
	template<class T, class...Ts>
	void _get_columns(sqlite3_stmt* statement, RecordType& record)
	{
		constexpr size_t column_index = COLUMN_COUNT - sizeof...(Ts) - 1;
		static_assert(column_index < COLUMN_COUNT, "invalid column index");

		auto value{ _get_column<T>(statement, column_index) };
		record.emplace_back(std::move(value));

		if constexpr (sizeof...(Ts) > 0) {
			_get_columns<Ts...>(statement, record);
		}
	}

	template<class T>
	ValueType _get_column(sqlite3_stmt* statement, size_t column_index)
	{
		static_assert(false, "not implemented type");

		return {};
	}

	template<>
	ValueType _get_column<int>(sqlite3_stmt* statement, size_t column_index)
	{
		return ::sqlite3_column_int(m_statement, static_cast<int>(column_index));
	}

	template<>
	ValueType _get_column<std::string>(sqlite3_stmt* statement, size_t column_index)
	{
		auto text{ ::sqlite3_column_text(m_statement, static_cast<int>(column_index)) };
		auto text_{ reinterpret_cast<const char*>(text) };

		return std::string{ text_ };
	}

	template<class T>
	size_t _get_hash(T t) const
	{
		return std::hash<T>{}(t);
	}
};

template<class ...Ts>
class RecordRange
{
	sqlite3* m_db{};
	std::string m_sql;

public:
	explicit RecordRange(sqlite3* db, std::string sql) : m_db{ db }, m_sql{ sql }
	{}

	auto begin()
	{
		static_assert(sizeof(this) == sizeof(size_t), "invalid size");

		return Iterator<sizeof...(Ts), Ts...>{ m_sql, m_db, reinterpret_cast<size_t>(this) };
	}

	auto end()
	{
		static_assert(sizeof(this) == sizeof(size_t), "invalid size");

		return Iterator<sizeof...(Ts), Ts...>{ m_sql, reinterpret_cast<size_t>(this) };
	}
};


