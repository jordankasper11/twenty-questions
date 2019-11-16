import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments';
import { BaseEntity, BaseEntityRequest, EntityResponse } from '@models';
import { map } from 'rxjs/operators';

export abstract class BaseService {
    private _baseUrl: string;

    constructor(private http: HttpClient) {
        this._baseUrl = `${environment.requestUrlPrefix}/api`;
    }

    protected httpGet<T>(relativePath: string, parameters?: HttpParams, responseType?: string): Observable<T> {
        return this.http
            .get<T>(this._baseUrl + relativePath, { params: parameters, responseType: <any>responseType })
            .pipe(map(response => this.deserialize(response)));
    }

    protected httpPost<T>(relativePath: string, body: any): Observable<T> {
        return this.http
            .post<T>(this._baseUrl + relativePath, body)
            .pipe(map(response => this.deserialize(response)));
    }

    protected httpPut<T>(relativePath: string, body: any): Observable<T> {
        return this.http
            .put<T>(this._baseUrl + relativePath, body)
            .pipe(map(response => this.deserialize(response)));
    }

    protected httpDelete<T>(relativePath: string, parameters?: HttpParams): Observable<T> {
        return this.http
            .delete<T>(this._baseUrl + relativePath, { params: parameters })
            .pipe(map(response => this.deserialize(response)));
    }

    protected getParams(request?: any) {
        let params = new HttpParams();

        if (typeof request == "object")
            Object.getOwnPropertyNames(request).forEach(key => params = params.set(key, request[key]));

        return params;
    }

    protected deserialize(value: any): any {
        if (value instanceof Array)
            (<Array<any>>value).forEach(i => i = this.deserialize(i));
        else if (value instanceof Object)
            Object.keys(value).forEach(key => {
                if (key.endsWith("Url")) {
                    let url: string = value[key];

                    value[key] = url ? `${environment.requestUrlPrefix}${url}` : null;
                }
                else
                    value[key] = this.deserialize(value[key]);
            });
        else if (typeof value == "string") {
            const dateRegex = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?Z?$/;

            if (dateRegex.test(value))
                value = new Date(value);
        }

        return value;
    }
}

export abstract class BaseEntityService<TEntity extends BaseEntity, TRequest extends BaseEntityRequest<BaseEntity>> extends BaseService {
    constructor(http: HttpClient, protected endPoint: string) {
        super(http);
    }

    get(id: string): Observable<TEntity> {
        return super.httpGet(this.endPoint + '/' + id);
    }

    query(request?: TRequest): Observable<EntityResponse<TEntity>> {
        let params = this.getParams(request);

        return super.httpGet(this.endPoint, params);
    }

    upsert(entity: TEntity): Observable<TEntity> {
        if (entity.id)
            return this.httpPut(this.endPoint, entity);
        else
            return this.httpPost(this.endPoint, entity);
    }

    delete(id: string): Observable<void> {
        let params = new HttpParams();

        params = params.set('id', id);

        return super.httpDelete(this.endPoint, params);
    }
}
