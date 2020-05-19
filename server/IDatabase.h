#pragma once

#include "IRecordRange.h"


struct IDataBase
{
public:
	virtual IRecordRange get_rank( size_t size ) const = 0;
};