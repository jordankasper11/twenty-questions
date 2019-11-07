export abstract class BaseEntity  {
    public id: string;
    public createdDate?: Date;
    public createdBy?: string;
    public modifiedDate?: Date;
    public modifiedBy?: string;
}

export enum SortDirection {
    ASC,
    DESC
}

export abstract class BaseEntityRequest<TEntity extends BaseEntity> {
    sortDirection: SortDirection;
    pageNumber: number;
    pageSize: number;

    constructor() {
        this.sortDirection = SortDirection.ASC;
        this.pageNumber = 1;
    }
}

export class EntityResponse<TEntity extends BaseEntity> {
    pageNumber: number;
    pageSize: number;
    totalRecords: number;
    items: Array<TEntity>;
}