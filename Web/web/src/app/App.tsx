import { RouterProvider } from 'react-router';
import { Toaster } from 'sonner';
import { router } from './routes';

function App() {
  return (
    <>
      <RouterProvider router={router} />
      <Toaster position="top-right" richColors />
    </>
  );
}

export default App;
