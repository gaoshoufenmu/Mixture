#pragma once

#ifndef UTILS_H
#define UTILS_H

#include <stdio.h>
#ifndef ABS
#define ABS(x) (((x) < 0)?-(x):(x))
#endif

extern int array_double(void** array, int n, int size);
#endif

