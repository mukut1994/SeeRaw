import React from "react";
import Paper from '@mui/material/Paper';
import { Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Collapse, IconButton } from "@mui/material";
import KeyboardArrowDownIcon from '@mui/icons-material/KeyboardArrowDown';
import KeyboardArrowUpIcon from '@mui/icons-material/KeyboardArrowUp';
import { createChildContext, IRendererContext, RenderFunction, SeeRawRender, SeeRawRender2 } from "./SeeRaw";

export const TableRenderer: RenderFunction<any> = (context) => {
    return (
      <TableContainer component={Paper}>
        <Table aria-label="collapsible table">
          <TableHead>

            <TableRow>
              <TableCell colSpan={6}>Type {context.type}</TableCell>
            </TableRow>

            <TableRow>
            {
                Object.keys(context.data).map(r => 
                    <TableCell key={r}>{r}</TableCell>
                )
            }
            </TableRow>

          </TableHead>

          <TableBody>
            <Row row={context.data} context={context}></Row>
          </TableBody>
          
        </Table>
      </TableContainer>
    );
}

function Row(props: { row: any, context: IRendererContext<any> }) {
    return (
      <React.Fragment>
        <TableRow hover>
            {
                Object.keys(props.row).map(r => 
                    <TableCell key={r}><SeeRawRender2 context={createChildContext(props.context, r)}/></TableCell>
                )
            }
        </TableRow>
      </React.Fragment>
    );
  }