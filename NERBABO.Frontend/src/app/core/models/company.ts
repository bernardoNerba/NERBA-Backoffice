export interface Company {
    id: number;
    name: string;
    address? : string;
    phoneNumber?: string;
    locality? : string;
    zipCode? : string;
    email?: string;
    ativitySector: string;
    size: string;
}