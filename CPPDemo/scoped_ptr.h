#pragma once
template<class T> class scoped_ptr {
private:
	T *ptr_;
	scoped_ptr(scoped_ptr const &);
	scoped_ptr & operator=(scoped_ptr const &);
	typedef scoped_ptr<T> this_type;

public:
	typedef T element_type;
	explicit scoped_ptr(T *p = 0) : ptr_(p) {}
	virtual ~scoped_ptr() { 
		delete ptr_;
	}
	void reset(T *p = 0) {
		delete ptr_;
		ptr_ = p;
	}
	T & operator*() const { return *ptr_; }
	T * operator->() const { return ptr_; }
	T * get() const { return ptr_; }
};
