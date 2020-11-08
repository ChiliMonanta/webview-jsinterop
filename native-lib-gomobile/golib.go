package golib

import "C"

import (
	"fmt"
	"math"
	"sort"
	"sync"
)

var count int
var mtx sync.Mutex

//export Add
func Add(a, b int) int {
	result := a + b
	fmt.Printf("[GO] Add %v + %v = %v\n", a, b, result)
	return result
}

//export Cosine
func Cosine(x float64) float64 {
	result := math.Cos(x)
	fmt.Printf("[GO] Cosine(%v) = %v\n", x, result)
	return result
}

//export Sort
func Sort(vals []int) {
	unsorted := make([]int, len(vals))
	copy(unsorted, vals)
	sort.Ints(vals)
	fmt.Printf("[GO] Sort(%v) = %v\n", unsorted, vals)
}

//export SortPtr
func SortPtr(vals *[]int) {
	Sort(*vals)
}

//export Log
func Log(msg string) int {
	mtx.Lock()
	defer mtx.Unlock()
	fmt.Println("[GO]", msg)
	count++
	return count
}

//export LogPtr
func LogPtr(msg *string) int {
	return Log(*msg);
}
