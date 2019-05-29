#include "stdafx.h"
#include "kdtree.h"
#include <stdio.h>
#include <malloc.h>
#include "minpq.h"
#include "feature.h"
#include "utils.h"

struct bbf_data
{
	double d;
	void* old_data;
};

static struct kd_node* kd_node_init(struct feature*, int);
static void expand_kd_node_subtree(struct kd_node*);
static struct kd_node* explore_to_leaf(struct kd_node*, struct feature*, struct min_pq*);
static int insert_into_nbr_array(struct feature*, struct feature**, int, int);

struct kd_node* kdtree_build(struct feature* features, int n)
{
	struct kd_node* kd_root;
	if (!features || n <= 0)
	{
		fprintf(stderr, "Warning: kdtree_build(): no features, %s, line %d\n", __FILE__, __LINE__);
		return NULL;
	}

	kd_root = kd_node_init(features, n);
	expand_kd_node_subtree(kd_root);
	return kd_root;
}

/**
	finds an image feature's approximate k nearest neighbors in a k-d tree using Best Bin First search

	@param kd_root root of an image feature k-d tree
	@param feat image feature for whose neighbors to search
	@param k number of neighbors to find
	@param nbrs pointer to an array in which to store pointers to neighbors in order of increasing descriptor distance
	@param max_nn_chks search is cut off after examining this many tree entries

	@return returns the number of neighbors found and stored in nbrs, or -1 on error
*/
int kdtree_bbf_knn(struct kd_node* kd_root, struct feature* feat, int k, struct feature*** nbrs, int max_nn_chks)
{
	struct kd_node* expl;
	struct min_pq* min_pq;
	struct feature ** _nbrs;
	struct bbf_data* bbf_data_;
	int i, t = 0, n = 0;

	if (!nbrs || !feat || !kd_root)
	{
		fprintf(stderr, "Warning: NULL pointer error, %s, line %d\n", __FILE__, __LINE__);
		return -1;
	}

	_nbrs = (feature **)calloc(k, sizeof(struct feature*));
	min_pq = minpq_init();
	minpq_insert(min_pq, kd_root, 0);
	while (min_pq->n > 0 && t < max_nn_chks)
	{
		expl = (kd_node*)minpq_extract_min(min_pq);
		if (!expl)
		{
			fprintf(stderr, "Warning: PQ unexpectedly empty, %s line %d\n", __FILE__, __LINE__);
			goto fail;
		}
		expl = explore_to_leaf(expl, feat, min_pq);
		if (!expl)
		{
			fprintf(stderr, "Warning: PQ unexpectedly empty, %s line %d\n", __FILE__, __LINE__);
			goto fail;
		}

		for (int i = 0; i < expl->n; i++)
		{
			struct feature* tree_feat = &(expl->features[i]);
			bbf_data_ = (bbf_data *)malloc(sizeof(struct bbf_data));
			if (!bbf_data_)
			{
				fprintf(stderr, "Warning: unable to allocate memory", " %s line %d\n", __FILE__, __LINE__);
				goto fail;
			}
			//bbf_data_->old_data = tree_feat->data;
			bbf_data_->d = descr_dist_sq(feat, tree_feat);
			tree_feat->data = bbf_data_;
			n += insert_into_nbr_array(tree_feat, _nbrs, n, k);
		}
		t++;
	}
	*nbrs = _nbrs;
	return n;
fail:
	minpq_release(&min_pq);
	return -1;
}


static struct kd_node* explore_to_leaf(struct kd_node* kd_node, struct feature* feat, struct min_pq* min_pq)
{
	struct kd_node* unexpl, *expl = kd_node;
	double kv;
	int ki;

	while (expl && !expl->leaf)
	{
		ki = expl->ki;
		kv = expl->kv;

		if (feat->x[ki] <= kv)		// destination <= median value
		{
			unexpl = expl->kd_right;		// unexplored child
			expl = expl->kd_left;			// explored child
		}
		else
		{
			unexpl = expl->kd_left;
			expl = expl->kd_right;
		}
		if (minpq_insert(min_pq, unexpl, ABS(kv - feat->x[ki])))
		{
			fprintf(stderr, "Warning: unable to insert into PQ, %s line %d\n", __FILE__, __LINE__);
			return NULL;
		}
	}
	return expl;
}

/**
	inserts a feature into the nearest-neighbor array so that the array remains in order of increasing
	descriptor distance from the search feature

	@param feat feature to be inserted into the array;
	@param nbrs array of nearest neighbors
	@param n number of elements already in nbrs and
	@param k maximum number of elements in nbrs
*/
static int insert_into_nbr_array(struct feature* feat, struct feature** nbrs, int n, int k)
{
	struct bbf_data* fdata, *ndata;
	double dn, df;
	int i, ret = 0;

	if (n == 0)
	{
		nbrs[0] = feat;
		return 1;
	}

	fdata = (bbf_data*)feat->data;
	df = fdata->d;
	ndata = (bbf_data*)nbrs[n - 1]->data;
	dn = ndata->d;
	if (df >= dn)
	{
		if (n == k)
		{
			return 0;
		}
		nbrs[n] = feat;
		return 1;
	}

	if (n < k)
	{
		nbrs[n] = nbrs[n - 1];
		ret = 1;
	}
	else
	{
		// n == k
	}
	i = n - 2;
	while (i > -1)
	{
		ndata = (bbf_data*)nbrs[i]->data;
		dn = ndata->d;
		if (dn <= df)
			break;

		nbrs[i + 1] = nbrs[i];
		i--;
	}
	i++;
	nbrs[i] = feat;
	return ret;
}


