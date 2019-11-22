import { BaseTrackedEntity, BaseRequest, BaseEntityRequest } from './base.models';

export class UserEntity extends BaseTrackedEntity {
    public username: string;
    public email: string;
    public avatarUrl: string;
}

export class UserRequest extends BaseEntityRequest<UserEntity> {
    public username: string;
}

export class RegistrationRequest extends BaseRequest {
    public username: string;
    public email: string;
    public password: string;
}

export class UpdateSettingsRequest extends BaseRequest {
    public userId: string;
    public username: string;
    public email: string;
    public password: string;
    public newPassword: string;
}