import * as React from 'react';
import Button from '@mui/material/Button';
import DialogTitle from '@mui/material/DialogTitle';
import Drawer from '@mui/material/Drawer';
import FormGroup from '@mui/material/FormGroup';
import TextField from '@mui/material/TextField';
import Stack from '@mui/material/Stack';
import Box from '@mui/material/Box';
import BasicSelect from './BasicSelect';
import InputLabel from '@mui/material/InputLabel';
import MenuItem from '@mui/material/MenuItem';
import FormControl from '@mui/material/FormControl';
import Select from '@mui/material/Select';

function AddSelect(props) {
    return (
        <Box sx={{ minWidth: 120 }}>
            <FormControl fullWidth>
                <InputLabel id={"select-label-" + props.header}>{props.label}</InputLabel>
                <Select
                    labelId={"select-label-" + props.header}
                    id={"select-" + props.header}
                    value={props.state}
                    label={props.label}
                    onChange={props.handleChange}
                >
                    {props.data.map((item) =>
                        <MenuItem key={item.id} value={item.id}>{item[props.header]}</MenuItem>
                    )}
                </Select>
            </FormControl>
        </Box>
    );
}


function getCurrentDate(separator = '') {

    let newDate = new Date()
    let date = newDate.getDate();
    let month = newDate.getMonth() + 1;
    let year = newDate.getFullYear();

    return `${year}${separator}${month < 10 ? `0${month}` : `${month}`}${separator}${date}`
}

function SimpleDialog(props) {
    const { onClose, selectedValue, open } = props;

    const handleClose = () => {
        onClose(selectedValue);
    };

    const [data, setData] = React.useState([]);

    const [type, setType] = React.useState(1);

    const [selectState1, setSelectState1] = React.useState("");
    const [selectState2, setSelectState2] = React.useState("");

    const handleTextChange = (event) => {
        let label = event.label;
        setData({
            [label]: event.target.value
        })
        console.log(data);
    };

    const handleSelectChange = (event) => {
        setType(event.target.value)
    };

    const handleSelectChange1 = (event) => {
        setSelectState1(event.target.value)
    };


    const handleSelectChange2 = (event) => {
        setSelectState2(event.target.value)
    };

    const handleSubmit = (event) => {
        event.preventDefault();
        if (type == 1) {

        }
    }

    let title;
    if (type == 1) {
        title = "Создание задачи";
    } else {
        title = "Создание проекта";
    }

    console.log("props headers in add modal", props.headers)

    return (
        <Drawer anchor="right" onClose={handleClose} open={open}>
            <DialogTitle>{title}</DialogTitle>
            <Box sx={{
                padding: 2,
                minWidth: "400px"
            }}>

                <FormGroup>
                    <Stack spacing={2} component="form" onSubmit={handleSubmit}>
                        <BasicSelect handleChange={handleSelectChange} type={type} />
                        {props.headers.map((field) => field.createAvailability &&
                            <>
                                {field.type === "datefield" &&
                                    <TextField
                                        id={field.name}
                                        label={field.title}
                                        type="date"
                                        onChange={(e) => {
                                            e.label = field.title
                                            handleTextChange(e)
                                        }}
                                        defaultValue={getCurrentDate('-')}
                                        InputLabelProps={{
                                            shrink: true,
                                        }}
                                    />
                                }
                                {field.type === "timefield" &&
                                    <TextField
                                        id={field.name}
                                        label={field.title}
                                        type="time"
                                        inputProps={{
                                            step: 1,
                                        }}
                                        onChange={(e) => {
                                            e.label = field.title
                                            handleTextChange(e)
                                        }}
                                        defaultValue="00:00:00"
                                        InputLabelProps={{
                                            shrink: true,
                                        }}
                                    />
                                }
                                {field.type === "textfield" &&
                                    <TextField id={field.name} label={field.title} variant="outlined" multiline onChange={(e) => {
                                        e.label = field.title
                                        handleTextChange(e)
                                    }} />
                                }
                                {field.type === "select" &&
                                    <AddSelect
                                        label={field.title}
                                        handleChange={field.fieldToShow === "name" ? handleSelectChange1 : handleSelectChange2}
                                        data={props[field.name]}
                                        header={field.fieldToShow}
                                        state={field.fieldToShow === "name" ? selectState1 : selectState2}
                                    />
                                }

                            </>
                        )}
                        <Button variant="contained" onClick={handleClose}>Создать</Button>
                    </Stack>
                </FormGroup>

            </Box>
        </Drawer>
    );
}

export default function TaskAddModal(props) {

    const { children } = props;

    const [open, setOpen] = React.useState(false);

    const handleClickOpen = () => {
        setOpen(true);
    };

    const handleClose = (value) => {
        setOpen(false);
    };

    return (
        <>
            <Button style={{ minWidth: 180 }} variant="contained" onClick={handleClickOpen}>Добавить&nbsp;задачу</Button>
            <SimpleDialog
                open={open}
                onClose={handleClose}
                headers={props.headers}
                supervisor={props.supervisor}
                projectCode={props.projectCode}
            />
        </>
    );
}
