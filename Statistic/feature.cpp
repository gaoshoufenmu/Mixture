#include "stdafx.h"
#include "feature.h"
#include "utils.h"

double descr_dist_sq(struct feature* f1, struct feature* f2)
{
	double sum = 0;
	double diff = 0;
	for (int i = 0; i < f1->n; i++)
	{
		diff = f1->x[i] - f2->x[i];
		sum += diff * diff;
	}
	return sum;
}
