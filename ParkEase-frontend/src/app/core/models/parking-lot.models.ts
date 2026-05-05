export interface CreateParkingLotRequest {
  name: string;
  address: string;
  city: string;
  latitude: number;
  longitude: number;
  totalSpots: number;
  availableSpots?: number;
  managerId: number;
  isOpen: boolean;
  isApproved: boolean;
  openTime: string; // HH:mm
  closeTime: string; // HH:mm
  imageFile?: File;
}

export interface UpdateParkingLotRequest {
  name?: string;
  address?: string;
  city?: string;
  latitude?: number;
  longitude?: number;
  totalSpots?: number;
  availableSpots?: number;
  managerId?: number;
  isOpen?: boolean;
  isApproved?: boolean;
  openTime?: string;
  closeTime?: string;
  imageFile?: File;
}

export interface ParkingLotResponse {
  lotId: number;
  name: string;
  address: string;
  city: string;
  latitude: number;
  longitude: number;
  totalSpots: number;
  availableSpots: number;
  managerId: number;
  isOpen: boolean;
  isApproved: boolean;
  openTime: string;
  closeTime: string;
  imageUrl?: string;
  createdAt: string;
  updatedAt: string;
}
