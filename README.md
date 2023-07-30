# marketDataNotification

This is a CLI application for monitoring market data using AlphaVantage API. Lower bound(buy threshold) and upper bound(sell threshold) can be set for triggering a email.

Usage:
```
marketDataNotification.exe [<ticker> <lowerLimit> <upperLimit>]
```
Example:
```
marketDataNotification.exe PETR4.SA 22.63 23.55
```
When the bounds are crossed the following email will be sent for the configures recipients:
```
Subject: Upper bound reached for PETR4.SA
```
```
The upper limit of 23.55 for PETR4.SA was reached. Current price is 29.7600. Asset sale is recomended 
```