#include "stdafx.h"
#include "utils.h"
#include <malloc.h>

/**
	double the size of an array with error checking

	@param array pointer to an array whose size is to be doubled
	@param n number of elements allocated for \a array
	@param size size in bytes of elements in \a array

	@return returns the new number of elements allocated for \a array
*/
int array_double(void** array, int n, int size)
{
	void* tmp;
	tmp = realloc(*array, 2 * n * size);
	if (!tmp)
	{
		fprintf(stderr, "Warning: unable to allocate memory in array_double()," " %s line %d \n", __FILE__, __LINE__);
		if (*array)
		{
			free(*array);
		}
		*array = NULL;
		return 0;
	}
	*array = tmp;
	return n * 2;
}