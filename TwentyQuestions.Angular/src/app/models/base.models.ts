export enum EntityStatus {
    Active = 1,
    Archived = 2,
    Deleted = 3
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
    sortDirection: SortDirection;
    pageNumber: number;
    pageSize: number;

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