import React from 'react';
import './App.css';
import { TableRenderer } from './TableRenderer';
import { IRendererContext, ISeeRawContext, NumberRenderer, SeeRawContext, SeeRawRender, SeeRawRender2, StringRenderer } from './SeeRaw';
import { NavigationRenderer } from './NavigationRenderer';

function App() {
  const context: ISeeRawContext = {
    renderers: {
      "default": TableRenderer,
      "string": StringRenderer,
      "number": NumberRenderer,
      "navigation": NavigationRenderer
    }
  };

  const root: IRendererContext<any> = {
    data: { name: "OK", fat: 1, x: { name: "OK", fat: 1 } } ,
    type: "root",

    rendererName: "navigation",
    rendererConfig: null
  }

  return (
    <SeeRawContext.Provider value={context} >
      <SeeRawRender2 context={root}  />
    </SeeRawContext.Provider>
  );
}

export default App;
