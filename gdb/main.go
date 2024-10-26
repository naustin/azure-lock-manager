package main

import (
    "fmt"
    "encoding/json"
    "os"
)


func filter(data []string, f func(string) bool) []string {

    fltd := make([]string, 0)

    for _, e := range data {

        if f(e) {
            fltd = append(fltd, e)
        }
    }

    return fltd
}

func main() {

    type LockItems []struct {
        ResourceGroupName string `json:"resourceGroupName"`
        SkipLocking       string `json:"skipLocking"`
        TTLUnitEpoch      string `json:"ttl_unit_epoch"`
    }
    
    file, err := os.ReadFile("db.json")
    if err != nil {
     fmt.Println("Error reading josn file", err)
    }
    
    var gdb LockItems
    
    err = json.Unmarshal([]byte(file), &gdb)
    if err != nil {
     fmt.Println("Error unmarshalling data", err)
     return
    }
    fmt.Println(gdb)
    
}