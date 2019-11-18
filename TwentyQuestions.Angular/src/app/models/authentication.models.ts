import { BaseRequest, BaseResponse, BaseTrackedEntity } from './base.models';

export class RegistrationRequest extends BaseRequest {
    public username: string;
    public email: string;
    public password: string;
}

export class LoginRequest extends BaseRequest {
    public username: string;
    public password: string;

    constructor(username: string, password: string) {
        super();

        this.username = username;
        this.password = password;
    }
}

export class LoginResponse extends BaseResponse {
    public accessToken: string;
    public refreshToken: string;

    constructor() {
        super();
    }
}

export class RefreshTokenRequest extends BaseRequest {
    public refreshToken: string;

    constructor() {
        super();
    }
}

export interface AccessToken {
    userId: string;
    username: string;
    exp: number;
}