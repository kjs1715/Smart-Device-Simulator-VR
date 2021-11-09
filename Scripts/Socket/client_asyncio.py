import asyncio
import json
import csv
import prettytable as pt

counter = 0
isLog = False
dataOutput = False
setCoefficients = False
logFileIndex = 0
headers = ['Score', 'info', 'Content', 'Method', 'Channel' , 'Device', 'Size', 'Availibility', 'Visibility', 'Distance', 'Angle']
logHeaders = ['Index', 'Time']

# Asynchronous communication with Unity 3D over TCP
async def tcp_echo_client(message, loop):
    # open connection with Unity 3D
    # reader, writer = await asyncio.open_connection('192.168.21.180', 8081, loop=loop)
    # reader, writer = await asyncio.open_connection('192.168.20.126', 8081, loop=loop)

    # reader, writer = await asyncio.open_connection("192.168.11.153", 8081, loop=loop)

    reader, writer = await asyncio.open_connection('192.168.31.79', 8082, loop=loop)
    # reader, writer = await asyncio.open_connection('192.168.31.150', 8082, loop=loop)

    # reader, writer = await asyncio.open_connection('183.173.132.85', 8081, loop=loop)

    print('Send: %r' % message)

    # test purposes
    global counter
    global isLog
    global dataOutput
    global setCoefficients
    global logFileIndex

    global headers
    # message['keydown'] = f'Hello World {counter}!'

    # convert JSON to bytes
    message_json = json.dumps(message).encode()
    # send message
    writer.write(message_json)
    counter += 1

    # wait for data from Unity 3D
    data = await reader.read(100000)
    # print(type(data))
    # we expect data to be JSON formatted
    data = data.decode()
    data_json = json.loads(data, strict=False)
    


    # use pretty table to output
    tb = pt.PrettyTable(["Device Num", "Device", "Texture"])

    if isLog:
        # fileName = 'C:/Users/Jinseo Kim/Desktop/Projects/TutorialVRsimulator/Assets/Resources/Experiment/CSV/'
        with open("log" + str(logFileIndex) + ".csv", 'w', newline='') as f:
            f_csv = csv.writer(f)
            f_csv.writerow(logHeaders)
            for key in data_json:
                for k in data_json[key]:
                    row = []
                    row.append(k)
                    row.append(data_json[key][k])
                    f_csv.writerow(row)
                    print(k, data_json[key][k])
        isLog = False
        logFileIndex += 1
        f.close()
    elif dataOutput:
        with open("test1.csv", 'w', newline='') as f:
            f_csv = csv.writer(f)
            f_csv.writerow(headers)
            for key in data_json:
                for k in data_json[key]:
                    temp = data_json[key][k].split(' ')
                    keys = list(temp[0].split(','))
                    values = list(temp[1].split(','))
                    newList = keys + values
                    f_csv.writerow(newList)
        f.close()
                
        dataOutput = False
    else:
        count = 0
        for key in data_json:
            for k in data_json[key]:
                tb.add_row([str(count), k, data_json[key][k]])
            count += 1
        print(tb)

    print('Close the socket')
    writer.close()

# def cmd_mapping(command):

def ReadCSVfile():
    file_name = r'C:\Users\Jinseo Kim\Desktop\Projects\TutorialVRsimulator\Assets\Resources\Experiment\CSV\final_coefficients.csv'
    gt_file_name = r'C:\Users\Jinseo Kim\Desktop\Projects\TutorialVRsimulator\Assets\Resources\Experiment\CSV\final_gt.csv'

    csv_str = ''

    with open(file_name, 'r') as f:
        reader = csv.reader(f)
        header_row = next(reader)
        print(header_row)
        for row in reader:
            for c in row:
                csv_str += c + ' '
    f.close()

    csv_str += ','

    with open(gt_file_name, 'r') as f:
        reader = csv.reader(f)
        header_row = next(reader)
        print(header_row)
        for row in reader:
            csv_str += row[3] + ' ' + row[4] + ' ' + row[5] + ' '
    f.close()
        
    return csv_str 



# src dst
message = {}
loop = asyncio.get_event_loop()
while True:
    print("******** Operations ********")
    cmd = input('input oper :')
    temp = '0'
    if cmd == ';':
        isLog = True
    elif cmd == '=':
        dataOutput = True
    elif cmd == 'setc':
        # setCoefficients = True
        temp = ReadCSVfile()
    message[cmd] = temp

    loop.run_until_complete(tcp_echo_client(message, loop))
    message.clear()
loop.close()


ReadCSVfile()


# r : request for updated device chart
# s : stop or start scene
# num1 num2: srt dst -> change textures and task
