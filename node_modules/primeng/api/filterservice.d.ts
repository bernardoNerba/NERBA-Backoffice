import * as i0 from "@angular/core";
export declare class FilterService {
    filter(value: any[], fields: any[], filterValue: any, filterMatchMode: string, filterLocale?: string): any[];
    filters: {
        [rule: string]: Function;
    };
    register(rule: string, fn: Function): void;
    static ɵfac: i0.ɵɵFactoryDeclaration<FilterService, never>;
    static ɵprov: i0.ɵɵInjectableDeclaration<FilterService>;
}
