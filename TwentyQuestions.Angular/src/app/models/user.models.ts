import { BaseTrackedEntity, BaseEntityRequest } from './base.models';

export class UserEntity extends BaseTrackedEntity {
    public username: string;
    public email: string;
    public avatarUrl: string;
}

export class UserRequest extends BaseEntityRequest<UserEntity> {
    public username: string;
}