#pragma once

#include <algorithm>
#include <fstream>
#include <iostream>
#include <new>
#include <random>
#include <sstream>
#include <string>
#include <tuple>
#include <variant>
#include <vector>

#include <cpprest/asyncrt_utils.h>
#include <cpprest/http_client.h>
#include <cpprest/http_listener.h>
#include <cpprest/json.h>
#include <cpprest/uri.h>
#include <sqlite3.h>

#ifdef _WIN32
#include <Windows.h>
#else
#include <sys/time.h>
#endif

#include <assert.h>