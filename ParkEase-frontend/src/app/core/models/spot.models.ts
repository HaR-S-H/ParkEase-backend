export enum SpotType {
  COMPACT = 0,
  STANDARD = 1,
  LARGE = 2,
  MOTORBIKE = 3,
  EV = 4
}

export enum VehicleType {
  TWOW = 0,
  FOURW = 1,
  HEAVY = 2
}

export enum SpotStatus {
  AVAILABLE = 0,
  RESERVED = 1,
  OCCUPIED = 2
}

export interface ParkingSpotResponse {
  spotId: number;
  lotId: number;
  spotNumber: string;
  floor: number;
  spotType: SpotType;
  vehicleType: VehicleType;
  status: SpotStatus;
  isHandicapped: boolean;
  isEVCharging: boolean;
  pricePerHour: number;
  createdAt: string;
  updatedAt: string;
}
