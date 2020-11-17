#pragma once

using namespace web;
using namespace utility;
using namespace http;
using namespace web::http::experimental::listener;

class Database;


class Server
{
public:
	explicit Server(web::uri_builder& uri);
	Server() = default;

	void wait();

private:
	void handle_get_rank(http_request);
	void handle_put_rank(http_request);

private:
	uri_builder m_uri;
	http_listener m_listener_get_rank;
	http_listener m_listener_put_rank;
	std::unique_ptr<Database> m_db;
};