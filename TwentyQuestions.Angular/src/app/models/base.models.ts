export enum EntityStatus {
    Active = 1,
    Pending = 2,
    Archived = 4,
    Deleted = 8
}

export enum SortDirection {
    ASC,
    DESC
}

export abstract class BaseEntity {
    public id: string;
    public status: EntityStatus;
}

export abstract class BaseTrackedEntity extends BaseEntity {
    public createdDate?: Date;
    public createdBy?: string;
    public modifiedDate?: Date;
    public modifiedBy?: string;
}

export class BaseRequest {
}

export abstract class BaseEntityRequest<TEntity extends BaseEntity> extends BaseRequest {
    public ids: Array<string>;
    public status: EntityStatus;
    public sortDirection: SortDirection;
    public pageNumber: number;
    public pageSize: number;

    constructor() {
        super();

        this.sortDirection = SortDirection.ASC;
        this.pageNumber = 1;
    }
}

export abstract class BaseResponse {
}

export class EntityResponse<TEntity extends BaseEntity> extends BaseResponse {
    pageNumber: number;
    pageSize: number;
    totalRecords: number;
    items: Array<TEntity>;
}