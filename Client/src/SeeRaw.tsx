import styled from "@emotion/styled";
import { createContext, useContext } from "react";

export interface IRendererContext<T>
{
    type: string;
    data: any;

    rendererName: string;
    rendererConfig: T | null;
}

export interface RenderFunction<T> {
    (context: IRendererContext<T>): React.ReactNode;
}

export const SeeRawContext = createContext<ISeeRawContext | null>(null);

export interface ISeeRawContext {
    renderers: { [rendererName: string]: RenderFunction<any> }
}

export function useSeeRaw() : ISeeRawContext { 
    const context = useContext(SeeRawContext);

    if(!context) throw new Error('Not in a seeRaw renderer context');

    return context;
}

export interface SeeRawRenderProps extends IRendererContext<any> {
    child: string | number;
}

export const SeeRawRender = (props: SeeRawRenderProps) => {
    const seeRaw = useSeeRaw();
    const childContext = createChildContext(props, props.child);

    return <>{seeRaw.renderers[childContext.rendererName](childContext)}</>
}

export const SeeRawRender2 = (props: { context: IRendererContext<any> }) => {
    const seeRaw = useSeeRaw();

    return <>{seeRaw.renderers[props.context.rendererName](props.context)}</>
}

export function createChildContext(context: IRendererContext<any>, child: string | number) : IRendererContext<any> {

    let childContext: IRendererContext<any> = {
        type: "default",
        data: context.data[child],

        rendererName: "default",
        rendererConfig: null,
    }

    if(typeof(context.data[child]) === "string") {
        childContext.rendererName = "string"; 
    }
    else if(typeof(context.data[child]) === "number") {
        childContext.rendererName = "number"; 
    }

    return childContext;
}

const StyledStringSpan = styled.span`
    color: red
`

export const StringRenderer: RenderFunction<any> = (context) => <StyledStringSpan>{context.data}</StyledStringSpan>

const StyledNumberSpan = styled.span`
    color: blue
`

export const NumberRenderer: RenderFunction<any> = (context) => <StyledNumberSpan>{context.data}</StyledNumberSpan>