#pragma once

#ifndef KDTREE_H
#define KDTREE_H

struct feature;

/** a node in a k-d tree */
struct kd_node
{
	int ki;						/**< partition key index */
	double kv;					/**< partition key value */
	int leaf;					/**< 1 if node is leaf, 0 otherwise */
	struct feature * features;	/**< features at this node */
	int n;						/**< number of features */
	struct kd_node* kd_left;	/**< left child */
	struct kd_node* kd_right;	/**< right child */
};

/**
	A function to build a k-d tree
	@param features an array of features
	@param n the number of features in a feature

	@return Returns the root of a k-d tree built from features
*/
extern struct kd_node* kdtree_build(struct feature* features, int n);

/**
	Finds an image feature's approximate k nearest neighbors in a k-d tree using Best Bin First search

	@param kd_root root of an image feature k-d tree
	@param feat image feature for whose neighbors to search
	@param nbrs pointer to an array in which to store pointers to neighbors in order of increasing descriptor distance;
		memory for this array is allocated by this function and must be freed by the caller using free(*nbrs)
	@param max_nn_chks search is cut off after examining this many tree entries
	@return Returns the number of neighbors found and stored in \a nbrs, or -1 on error
*/
extern int kdtree_bbf_knn(struct kd_node* kd_root, struct feature* feat, int k, struct feature*** nbrs, int max_nn_chks);

//extern int kdtree_bbf_spatial_knn(struct kd_node* kd_root, struct feature* feat, int k, struct feature*** nbrs, int max_nn_chk, 

/**
	De-allocates memory held by a k-d tree
	@param kd_root pointer to the root of a k-d tree
*/
extern void kdtree_release(struct kd_node* kd_root);

#endif