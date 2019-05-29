#pragma once

#ifndef FEATURE_H
#define FEATURE_H


struct feature
{
	double * x;		/** dimession value array */
	int n;			/** dimession */
	void* data;		/** user custom data */
};


extern double descr_dist_sq(struct feature* f1, struct feature* f2);
#endif

