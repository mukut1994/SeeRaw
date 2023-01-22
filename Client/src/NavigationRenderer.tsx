import styled from "@emotion/styled";
import {
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  ListSubheader,
} from "@mui/material";
import { ReactElement, useState, Dispatch, SetStateAction } from "react";
import {
  createChildContext,
  IRendererContext,
  RenderFunction,
  SeeRawRender2,
} from "./SeeRaw";

const NavigationContainer = styled.div`
  display: float;
`;

const NavigationLeft = styled.div`
  float: left;
  padding-left: 1rem;
`;

const NavigationRight = styled.div`
  float: left;
  padding: 1rem;
`;

interface INavigationState {
  path: string[];
  contentContext: IRendererContext<any>;
}

export const NavigationRenderer: RenderFunction<any> = (context) => {
  const firstKey = Object.keys(context.data)[0];
  const [navigationState, setNavigationState] = useState<INavigationState>({
    path: [firstKey],
    contentContext: createChildContext(context, firstKey),
  });

  return (
    <NavigationContainer>
      <NavigationLeft>
        <List
          component="nav"
          aria-labelledby="nested-list-subheader"
          subheader={
            <ListSubheader component="div" id="nested-list-subheader">
              Type {context.type}
            </ListSubheader>
          }
        >
          <ListItems
            path={[firstKey]}
            navigationState={navigationState}
            index={0}
            setNavigationState={setNavigationState}
            context={context}
          />
        </List>
      </NavigationLeft>
      <NavigationRight>
        <SeeRawRender2 context={navigationState.contentContext} />
      </NavigationRight>
    </NavigationContainer>
  );
};

const ListCollection = styled.div`
  border-left: 0.2rem solid green;
`;

const ChildNav = styled.div`
  padding-left: 1rem;
`;

function ListItems(props: {
  path: string[];
  setNavigationState: Dispatch<SetStateAction<INavigationState>>;
  navigationState: INavigationState;
  index: number;
  context: IRendererContext<any>;
}) {
  let ret: ReactElement[] = [];

  Object.keys(props.context.data).forEach((key) => {
    const keyContext = createChildContext(props.context, key);

    if (key === "x") {
      ret.push(
        <>
          <ListItem key={"$" + key}>
            <ListItemText key={"$2" + key} primary={key} />
          </ListItem>
          <ChildNav key={"$C" + key}>
            <ListItems
              key={key}
              context={keyContext}
              index={props.index + 1}
              path={[...props.path, key]}
              navigationState={props.navigationState}
              setNavigationState={props.setNavigationState}
            />
          </ChildNav>
        </>
      );
    } else {
      ret.push(
        <ListItemButton
          key={key}
          selected={props.navigationState.path[props.index] === key}
          onClick={() =>
            props.setNavigationState({
              path: [...props.path.slice(0, -1), key],
              contentContext: keyContext,
            })
          }
        >
          <ListItemText primary={key} />
        </ListItemButton>
      );
    }
  });

  return <ListCollection key={"$asd" + props.path[props.index]}>{ret}</ListCollection>;
}
