package main

/*
#include <stdlib.h>
#include <stdint.h>

struct organization {
	char*             name;
	char*             registratedDate;
	double            solidity;
	struct employee* employees;
};

struct employee {
    char*    name;
	int64_t  age;
	struct employee* next;
};
*/
import "C"

import (
	"fmt"
	"time"
	"math"
	"sort"
	"sync"
	"encoding/json"
	"runtime"
	"unsafe"
)

var count int
var mtx sync.Mutex

type CustomError struct {
	Message string `json:"message"`
	Type string `json:"type"`
	StackTrace string `json:"stacktrace"`
}

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

//export DoPanic
func DoPanic() (err *C.char) {
	
	defer func() {
		if r := recover(); r != nil {
			stackSlice := make([]byte, 512) 
			s := runtime.Stack(stackSlice, false)
			var stacktrace = fmt.Sprintf("%s", stackSlice[0:s])

			var ce = CustomError{Message: fmt.Sprintf("%v", r), Type: "RunTimeError", StackTrace: stacktrace}
			var json, _ = json.Marshal(ce)
			err = C.CString(string(json))
		}
	}()

	DoPanicInternal()
	return nil
}

func DoPanicInternal() int{
	var a = 5
	var b = 1 / (5 - a)
	fmt.Println("[GO] supposed not to be printed")
	return b
}

//export DoThrow
func DoThrow() *C.char {
	var error = DoThrowInternal()
	var json, _ = json.Marshal(error)
	return C.CString(string(json))
}

func DoThrowInternal() CustomError {
	stackSlice := make([]byte, 512) 
	s := runtime.Stack(stackSlice, false)
	var stacktrace = fmt.Sprintf("%s", stackSlice[0:s])

	return CustomError{Message: "Item not found", Type: "ErrNotFound", StackTrace: stacktrace}
}

type Employee struct {
	Name 	string 	`json:"name"`
	Age 	int 	`json:"age"`
}

type Organization struct {
	Name 			string 		`json:"name"`
	RegistratedDate string 		`json:"registratedDate"`
	Solidity 		float64 	`json:"solidity"`
	Employees 		[]Employee 	`json:"employees"`
}

// You cant export a Go type
// https://golang.org/cmd/cgo/#hdr-C_references_to_Go
// "Not all Go types can be mapped to C types in a useful
// way. Go struct types are not supported; use a C struct type.
// Go array types are not supported; use a C pointer."

func GetOrganization(id string) Organization {
	var org = Organization { 
		Name: "Stark Industries",
		RegistratedDate: time.Now().Format(time.RFC3339),
		Solidity: 30.2,
		Employees: []Employee {
			Employee {
				Name: "Tony Stark",
				Age: 31,
			},
			Employee {
				Name: "Pepper Potts",
				Age: 30,
			},
		},
	}
	return org;
}

//export GetOrganizationAsJson
func GetOrganizationAsJson(id string) *C.char {
	var org = GetOrganization(id)
	var json, _ = json.Marshal(org)
	var cOrg = C.CString(string(json))
	return cOrg;
}

//export GetOrganizationAsCtype
func GetOrganizationAsCtype(id string) *C.struct_organization {
	var org = GetOrganization(id)

	cOrg := (*C.struct_organization) (C.malloc(C.size_t(unsafe.Sizeof(C.struct_organization{}))))

	cOrg.name = C.CString(org.Name)
	cOrg.registratedDate = C.CString(org.RegistratedDate)
	cOrg.solidity = C.double(org.Solidity)
	cOrg.employees = nil

	var cNextEmploee = &cOrg.employees
	for _, emp:= range org.Employees {
		cEmployee := (*C.struct_employee) (C.malloc(C.size_t(unsafe.Sizeof(C.struct_employee{}))))

		cEmployee.name = C.CString(emp.Name)
		cEmployee.age = C.int64_t(emp.Age)
		cEmployee.next = nil
		*cNextEmploee = cEmployee
		cNextEmploee = &cEmployee.next
	}
	return cOrg	
}

// Go string to C string
// The C string is allocated in the C heap using malloc.
// It is the caller's responsibility to arrange for it to be
// freed, such as by calling C.free (be sure to include stdlib.h
// if C.free is needed).
// func C.CString(string) *C.char

//export FreeString
func FreeString(cString *C.char) {
	fmt.Println("[GO] Free:", cString)
	C.free(unsafe.Pointer(cString))
}

//export FreeOrganizationAsCtype
func FreeOrganizationAsCtype(org *C.struct_organization) {
	fmt.Println("[GO] Free:", org)
	employee := org.employees
	for employee != nil {
		nextEmploee := employee.next
		C.free(unsafe.Pointer(employee.name))
		C.free(unsafe.Pointer(employee))
		employee = nextEmploee
	}

	C.free(unsafe.Pointer(org.name))
	C.free(unsafe.Pointer(org.registratedDate))
	C.free(unsafe.Pointer(org))
}

func main() {
}