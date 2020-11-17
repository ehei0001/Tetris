#include "stdafx.h"
#include "Database.h"
#include "Server.h"


#ifdef _WIN32
int wmain( int argc, wchar_t* argv[] )
#else
int main( int argc, char* argv[] )
#endif
{
	utility::string_t address = U("http://localhost:");
	utility::string_t port = U("34568");

	if (argc > 0) {
		if (argc > 1)
		{
			address = argv[1];
		}

		if (argc > 2)
		{
			port = argv[2];
		}
	}

	address.append(port);

	uri_builder uri(address);

	if (!uri.is_valid()) 
	{
		std::cerr << L"error: address is invalid" << std::endl;
		std::cout << L"usage: tetris-server.exe [host] [port]" << std::endl;
		return 1;
	}

	uri.append_path(U("tetris"));

	std::unique_ptr<Server> server;

	try 
	{
		server = std::make_unique<Server>(uri);
	}
	catch (std::invalid_argument& e) 
	{
		std::cerr << "invalid uri:" << e.what() << std::endl;
		std::cerr << "... usage: tetris-server.exe [ip] [port]" << std::endl;
		return 1;
	}
	
	try
	{
		server->wait();
	}
	catch (web::http::http_exception& e)
	{	 
		std::cerr << "Launching is failed" << std::endl;
		std::cerr << "exception: " << e.what() << std::endl;
		return 1;
	}

	std::cout << "Server running. Press ENTER to exit." << std::endl;

	std::string line;
	std::getline( std::cin, line );

	return {};
}