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

func ReadGdbFile(gdb *LockItems) {
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

func RemoveOldRecords(gdb LockItems) LockItems {
    currentTime := time.Now().Unix()
    log.Debug("currenttime: ",currentTime)
    var filteredItems LockItems

    for _, item := range gdb {
        if item.TTLUnixEpoch > currentTime {
            filteredItems = append(filteredItems, item)
        }
    }

    log.Debug(filteredItems)

    return filteredItems
}

func main() {

    log.SetLevel(log.DebugLevel)
	log.SetReportCaller(true)
    log.SetFormatter(UTCFormatter{&log.JSONFormatter{}})

    log.Debug("Starting Main.")
    
    var gdb LockItems

    ReadGdbFile(&gdb)

    RemoveOldRecords(gdb)
    
}


