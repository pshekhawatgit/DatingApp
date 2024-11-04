export interface Pagination{
    currentPage: number;
    itemsPerPage: number;
    totalPages: number;
    totalCount: number;
}

export class PaginatedResult<T>{
    result?: T;
    pagination?: Pagination;
}