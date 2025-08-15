import { ReactNode } from 'react';
import { AppBar, Toolbar, Typography, Button, Container, Drawer } from '@mui/material';
import { Link } from 'react-router-dom';

interface Props {
  children: ReactNode;
  drawerContent?: ReactNode;
  drawerOpen?: boolean;
  onDrawerClose?: () => void;
}

export default function PageLayout({ children, drawerContent, drawerOpen = false, onDrawerClose }: Props) {
  return (
    <>
      <AppBar position="static">
        <Toolbar>
          <Typography variant="h6" sx={{ flexGrow: 1 }}>
            Design Doc Service
          </Typography>
          <Button color="inherit" component={Link} to="/">Documents</Button>
          <Button color="inherit" component={Link} to="/create">Create</Button>
        </Toolbar>
      </AppBar>
      <Container sx={{ mt: 2 }}>{children}</Container>
      {drawerContent && (
        <Drawer anchor="right" open={drawerOpen} onClose={onDrawerClose}>
          {drawerContent}
        </Drawer>
      )}
    </>
  );
}
