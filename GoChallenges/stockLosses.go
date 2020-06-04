package main

import "fmt"
import "os"
import "bufio"
import "strings"
import "strconv"

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

func main() {
    scanner := bufio.NewScanner(os.Stdin)
    scanner.Buffer(make([]byte, 1000000), 1000000)

    var n int
    scanner.Scan()
    fmt.Sscan(scanner.Text(),&n)
    
    scanner.Scan()
    inputs := strings.Split(scanner.Text()," ")

    high := int64(0);
    maxLoss := int64(0);
    for i := 0; i < n; i++ {
        v,_ := strconv.ParseInt(inputs[i],10,32)
        _ = v

        diff := high - v;
        if diff > 0{
            maxLoss = max(maxLoss, diff);
        }
        high = max(high, v);
    }
    
    // fmt.Fprintln(os.Stderr, "Debug messages...")
    fmt.Println(-maxLoss)// Write answer to stdout
}

func max(a int64, b int64) int64{
    if a > b{
        return a;
    }
    return b;
}