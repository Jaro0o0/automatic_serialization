import Box from '@mui/material/Box';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';
import type { SelectChangeEvent } from '@mui/material/Select';
import Button from '@mui/material/Button';
import SendIcon from '@mui/icons-material/Send';
import TextField from '@mui/material/TextField';

//animations
import { motion } from 'framer-motion';
import {container , item} from '../../animations/cardAnimation'

import { useState } from 'react';

type FileFormat = 'txt' | 'xml' | 'csv';


function Card() {
    const [dataType, setDataType] = useState<FileFormat | ''>('');

        const handleChange = (event: SelectChangeEvent) => {
        setDataType(event.target.value as FileFormat);
    };


    const [downloadPath,setdownloadPath] = useState('');

   const handleDownloadPath = (event: React.ChangeEvent<HTMLInputElement>) => {
    setdownloadPath(event.target.value);
    }   


//fetch_Patch



// fetch_Data
    const fetchData = async () => {
        if (!dataType || !downloadPath.trim()) {
            return;
        }

         
        const res = await fetch(
            `http://localhost:5000/serialize/Chocolate/${encodeURIComponent(dataType)}`,
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    Path: downloadPath.trim(),
                })
            }
        );

        const result = await res.json();

        console.log(result);
    };

    


    //handlers
  

    return ( 
        <>
        <motion.div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 p-8 shadow-xl rounded-2xl w-full max-w-md min-h-[500px] flex flex-col justify-between bg-white"
            variants={container}
            initial="initial"
            animate="animate"
            exit="exit"
        >

                {/* TEXT_BOX */}
                <motion.div className='mb-6 text-center'
                    variants={item}
                >
                    <h1 className="text-2xl font-bold text-[#1976d2] uppercase">Automatic Serialization</h1>
                    <p className="text-gray-500 mt-1">Choose format and select path to save data</p>
                </motion.div>

                {/* Inputs_BOX */}
                <motion.div className='flex flex-col gap-5'
                
                    variants={item}
                >
                    <TextField id="outlined-basic" fullWidth required label="Save path" variant="outlined" helperText="Full local path where the backend should save the file" value={downloadPath} onChange={handleDownloadPath} />
                    <Box sx={{ minWidth: 120 }}>
                        <FormControl fullWidth>
                            <InputLabel id="demo-simple-select-label">Format type</InputLabel>
                            <Select
                            labelId="demo-simple-select-label"
                            id="demo-simple-select"
                            value={dataType}
                            label="Format type"
                            onChange={handleChange}
                            >
                            <MenuItem value={'txt'}>.txt</MenuItem>
                            <MenuItem value={'xml'}>.xml</MenuItem>
                            <MenuItem value={'csv'}>.csv</MenuItem>
                            </Select>
                        </FormControl>
                    </Box>
                </motion.div>

                <Button variant='contained' size='large' component={motion.button} disabled={!dataType || !downloadPath.trim()} onClick={fetchData} endIcon={<SendIcon />}
                    variants={item}
                >Send</Button>

        </motion.div>

        

</>
    );
}

export default Card;
