import { BaseEntity } from './base.models';

export class LoginRequest {
    constructor(public username: string, public password: string) {}
}

export interface AccessToken {
    userId: string;
    username: string;
    exp: number;
}

export class UserEntity extends BaseEntity {
    public username: string;
    public email: string;
    public avatarUrl: string;
}