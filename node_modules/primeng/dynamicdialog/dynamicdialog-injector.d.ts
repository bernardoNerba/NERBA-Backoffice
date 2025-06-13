import { InjectOptions, Injector, ProviderToken, InjectFlags } from '@angular/core';
export declare class DynamicDialogInjector implements Injector {
    private _parentInjector;
    private _additionalTokens;
    constructor(_parentInjector: Injector, _additionalTokens: WeakMap<any, any>);
    get<T>(token: ProviderToken<T>, notFoundValue?: T, options?: InjectOptions | InjectFlags): T;
}
