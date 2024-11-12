import pymeshlab
import re

def clean_objs(gen_obj_list : str) -> None:
    """
    Function to create separate material files from the vertex colored obj files.
    
    :param gen_obj_list: The string containing all of the generated objects
    :type gen_obj_list: str
    """

    ### Temp
    gen_object_start = re.search("generatedObjectList", response.text).start()
    gen_object_end = re.search("]", response.text[gen_object_start:]).start() + gen_object_start
    cleaner_files = re.split("file_name", response.text[gen_object_start:gen_object_end])[1::2]
    clean_objs([s[4:re.search(',', s).start()-1] for s in cleaner_files])
