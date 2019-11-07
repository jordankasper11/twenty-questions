import { BaseEntity } from './base.model';

export class LoginRequest {
    constructor(public username: string, public password: string) {}
}

export interface AccessToken {
    userId: string;
    username: string;
}

export class UserEntity extends BaseEntity {
    public username: string;
    public email: string;
    public avatarUrl: string;
}