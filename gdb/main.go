package main

import (
	"encoding/json"
	"os"
	"time"

	log "github.com/sirupsen/logrus"
)

type UTCFormatter struct {
    log.Formatter
}

func (u UTCFormatter) Format(e *log.Entry) ([]byte, error) {
    e.Time = e.Time.UTC()
    return u.Formatter.Format(e)
}

func filter(data []string, f func(string) bool) []string {

    fltd := make([]string, 0)

    for _, e := range data {

        if f(e) {
            fltd = append(fltd, e)
        }
    }

    return fltd
}

type LockItems []struct {
    ResourceGroupName string `json:"resourceGroupName"`
    TTLUnixEpoch      int64 `json:"ttl_unix_epoch"`
}

type LockItemsRgOnly []struct {
    ResourceGroupName string `json:"resourceGroupName"`
}

func readGdbFile(gdb *LockItems) {
    file, err := os.ReadFile("db.json")
    if err != nil {
        log.Panic(err)
    }

    err = json.Unmarshal([]byte(file), &gdb)
    if err != nil {
     log.Panic("Error unmarshalling gdb json data.", err)
     return
    }

    log.Debug(gdb)
}

func removeOldRecords(gdb LockItems) []string {
    currentTime := time.Now().Unix()
    log.Debug("currenttime: ",currentTime)
	var resourceGroups []string

	for _, item := range gdb {
		if item.TTLUnixEpoch > currentTime {
			resourceGroups = append(resourceGroups, item.ResourceGroupName)
		}
	}

    log.Debug(resourceGroups)

	return resourceGroups
}

func removeOldRecordsForDbUpdate(gdb LockItems) LockItems {
    currentTime := time.Now().Unix()
    var validItems LockItems

    for _, item := range gdb {
        if item.TTLUnixEpoch > currentTime {
            validItems = append(validItems, item)
        }
    }

    return validItems
}

func removeMatches(arr1, arr2 []string) []string {
    // Create a map to track elements in arr2
    elements := make(map[string]bool)
    for _, item := range arr2 {
        elements[item] = true
    }

    // Iterate through arr1 and add non-matching elements to result
    var result []string
    for _, item := range arr1 {
        if !elements[item] {
            result = append(result, item)
        }
    }

    return result
}

func main() {

    log.SetLevel(log.DebugLevel)
	log.SetReportCaller(true)
    log.SetFormatter(UTCFormatter{&log.JSONFormatter{}})

    log.Debug("Starting Main.")
    
    var gdb LockItems

    readGdbFile(&gdb)

    // TODO: Update json db file and send to storage

    removeOldRecords(gdb)
    
}


