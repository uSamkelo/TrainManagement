export interface IStop {
    stationName: string;
    distance: number;
    latitude: number;
    longitude: number;
}

export interface IRoute {
    trainId: string;
    stops: IStop[];
    currentStopIndex: number;
}

export interface IPosition {
    trainId: string;
    latitude: number;
    longitude: number;
    status: string;
    eta: string;
    nextStopEta?: string;
    trail?: Array<[number, number]>; // Array of [lat, lng] coordinates
    currentStop?: string;
    nextStop?: string;
}

export interface TrainUpdate {
    trainId: string;
    latitude: number;
    longitude: number;
    status: string;
    eta: string;
    nextStopEta?: string;
    timestamp: string;
    trail?: Array<[number, number]>;
    currentStop?: string;
    nextStop?: string;
}

export interface IStation {
    id: number;
    stationName: string;
    latitude: number;
    longitude: number;
}

export interface INextArrivalResult {
    trainId: string;
    routeName: string;
    stationName: string;
    arrivalTime: string;
    eta: string;
    latitude: number;
    longitude: number;
}