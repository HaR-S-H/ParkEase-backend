
## 4.2 ParkingLot-Service

ParkingLot-Service manages parking-facility profiles and operational state for the ParkEase platform. Each lot stores geolocation data for GPS-based discovery, operating hours, spot counts, and an approval flag. Available spots are expected to be updated atomically with EF Core optimistic concurrency so bookings, checkouts, and cancellations do not overwrite each other.

Admin approval is required before a lot becomes searchable by drivers.

| Component | Type | Key Attributes / Methods |
| --- | --- | --- |
| ParkingLot | C# Class (Entity / POCO) | LotId, Name, Address, City, Latitude, Longitude, TotalSpots, AvailableSpots, ManagerId, IsOpen, IsApproved, OpenTime, CloseTime, ImageUrl |
| IParkingLotRepository | C# Interface | FindByLotId(), FindByCity(), FindByManagerId(), FindByIsOpen(), FindNearby(), FindByAvailableSpotsGreaterThan(), CountByCity(), DeleteByLotId() |
| ParkingLotService | C# Class (Service Impl) | CreateLot(), GetLotById(), GetLotsByCity(), GetNearbyLots(), GetLotsByManager(), UpdateLot(), ToggleOpen(), DeleteLot(), DecrementAvailable(), IncrementAvailable(), SearchLots() |
| IParkingLotService | C# Interface | Declares all lot CRUD, geo-proximity search, open-toggle, approval, and available-count management operations |
| ParkingLotController | C# Class ([ApiController]) | Exposes `/api/v1/lots` endpoints: POST (create), GET (by id/city/manager/nearby/search), PUT (update/toggleOpen), DELETE |
