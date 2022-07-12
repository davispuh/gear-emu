#!/usr/bin/env bash
#Collect files to a zip for distribution of Gear project

set -o pipefail
shopt -s lastpipe

#===============================================
#Global configuration

#Directory for program deploy tree
declare -g destPath="distribution/GEAR_Emulator"
#Deploy groups
declare -a deployGroups=('Release Binaries' 'Info & Licensing' 'Images' 'Resourses' 'Plugins')
#Dictionary of origin of files of each group, relative to basePath parameter
declare -A filesFrom
#Dictionary of destination of files of each group, relative to basePath parameter
declare -A filesDest

#Note: must use ':' character as file or path separator, for filesFrom and filesDest
filesFrom['Release Binaries']="Gear/bin/Release/Gear.exe:Gear/bin/Release/Gear.exe.config:Gear/bin/Release/Gear.exe.manifest"
filesDest['Release Binaries']="all,$destPath"
filesFrom['Info & Licensing']="changelog.md:licence.txt:readme.md:Visual Studio 2019 Image Library EULA.rtf"
filesDest['Info & Licensing']="all,$destPath"
filesFrom['Images']="images/Gear_LogicProbe.png:images/Gear_VGA+LogicProbe.png"
filesDest['Images']="all,$destPath/images"
filesFrom['Resourses']="Gear/bin/Release/Resources/Plugin.dtd:Gear/bin/Release/Resources/PluginTemplate.cs"
filesDest['Resourses']="all,$destPath/Resources"
filesFrom['Plugins']="plug-ins/Plugins_notes.md:plug-ins/PinNoise.xml:plug-ins/SerialIO.xml:plug-ins/Stimulus.xml:plug-ins/Television.xml:plug-ins/test.stm:plug-ins/vgamonitor.xml"
filesDest['Plugins']="all,$destPath/Plugins"

#===============================================
#Global variables

#Version of this program
declare -r version='1.0'
#Program name without path
declare -g progName=${0##*/}
#Program description
declare -r programDescription="Collect files for distribution of Gear project."
#Base path for tree structure
declare -g basePath
#Remember original IFS
declare -r OLDIFS=$IFS

#===============================================
#Set global error level tags
declare -r -i err_OptionNotRecognized=1
declare -r -i err_MissingValue=2
declare -r -i err_InternalError=3
declare -r -i err_FileNotFound=4
declare -r -i err_ExtraParameters=5

#Definitions for format and colors
if which tput >/dev/null 2>&1 && [[ $(tput -T"$TERM" colors) -ge 8 ]]; then
   declare -x fmtBold="\e[1m"
   declare -x fmtUnd="\e[4m"
   declare -x fmtInv="\e[7m"
   declare -x fmtWarn="\e[33m"
   declare -x fmtUser="\e[94m"
   declare -x fmtErr="\e[31m"
   declare -x fmtOk="\e[32m"
   declare -x fmtReset="\e[0m"
   declare -x fmtSection="\e[1;44;33m"
fi

#===============================================
#Functions

#Show usage
function ShowUsage() {
   #generate the output
   cat <<EOF >&2
Usage:
  $progName DIR
  $progName [ -h | -v ]

  DIR : Base directory of Gear project

Options:
  -h | --help             Show usage.
  -v | --version          Show program version.
EOF
}

#Process parameters for main
#  $@ : all the parameters of the program
# Exit error level
#   err_OptionNotRecognized
#   err_MissingValue
#   err_FileNotFound
#   err_ExtraParameters
function ProcessParams() {
   #define script options
   local options='hv'
   local longopts='help,version'
   ! parsed=$(getopt --options=$options --longoptions=$longopts --name "$progName" -- "$@")
   if [[ ${PIPESTATUS[0]} -ne 0 ]]; then
      ShowUsage
      exit $err_OptionNotRecognized
   fi
   # read getopt’s output this way to handle the quoting right, setting value to $1.
   eval set -- "$parsed"
   while true; do
      case $1 in
      -h | --help)
         echo -e "${fmtBold}${programDescription}${fmtReset}"
         ShowUsage
         exit
         ;;
      -v | --version)
         echo "Version $version"
         exit
         ;;
      --)
         shift
         break
         ;;
      *)
         echo -e "${fmtErr}Option not recognized: '$1'.${fmtReset}" >&2
         ShowUsage
         exit $err_OptionNotRecognized
         ;;
      esac
   done
   #read positional parameter(s)
   if [[ $# == 1 ]]; then
      if [[ -d $1 ]]; then
         basePath=$(Expand2AbsolutePath $1)
      else
         echo -e "${fmtErr}Base directory not reached: '$1'. Aborting.${fmtReset}" >&2
         exit $err_FileNotFound
      fi
   elif [[ $# == 0 ]]; then
         echo -e "${fmtErr}No value given to parameter Base directory. Aborting.${fmtReset}" >&2
         ShowUsage
         exit $err_MissingValue
   else
      shift
      echo -e "${fmtErr}Extra parameter(s) '$*' given. Aborting.${fmtReset}" >&2
      ShowUsage
      exit $err_ExtraParameters
   fi
}

#Expand a file or directory to full path
#  $1 : file or directory to expand
# Returns - write to std the full path
# Returns - error level
#  0 : ok
#  1 : error
#
# Supported patterns:
#   ~/ | ~/Gear/       - home shortcut to directory
#   ../Gear/ | ./Gear/ - relative path to directory
#   /home/user/Gear/   - absolute path to directory
#   ../readme.md | ./readme.md     - relative path to file
#   ~/readme.md | ~/Gear/readme.md - home shortcut to file
#   /home/user/Gear/readme.md      - absolute path to file
function Expand2AbsolutePath() {
   local base
   local file
   local useTmp=false
   local par=${1//\'/}
   #search for root dir
   if [[ $par == / ]]; then
      echo "/"
      return 0
   fi
   #search for file starting with /
   if [[ $par =~ ^/[^/]+$ ]]; then
      echo $par
      return 0
   fi
   #replace ~ for homedir
   if [[ $1 =~ ^~ ]]; then
      par=${par//\~/$(eval echo "~$USER")}
   fi
   #search for ending with /
   if [[ $par =~ ^.*/$ ]]; then
      base=${par}tmp
      useTmp=true
   else
      base=$par
      file=$(basename $par)
   fi
   base=$(cd "$(dirname "$base")" >/dev/null 2>&1 || return; pwd -P)
   if [[ -z $base ]]; then
      return 1
   fi
   if $useTmp; then
      echo "${base}"
   else
      echo "${base}/${file}"
   fi
   return 0
}

#Check is valid as Associative array Parameter
# $1 : name of array
# Returns - error level
#  0 : valid and declared as associative array
#  1 : not valid name or not declared as associative array
function CheckAssocArrayParam() {
   if [[ -z $1 ]]; then
      echo -e "${fmtErr}Empty array given as parameter in '${FUNCNAME[0]}($*)', called from '${FUNCNAME[1]}()'. Aborting.${fmtReset}" >&2
      return 1
   fi
   if [[ "$(declare -p "$1")" =~ "declare -A" ]]; then
      return 0
   else
      echo -e "${fmtErr}Variable '$1' not defined as Assocciative Array, given as parameter in '${FUNCNAME[0]}($*)', called from '${FUNCNAME[1]}()'. Aborting.${fmtReset}" >&2
      return 1
   fi
}

#Process destination directories, saving to Dictionary name given a parameter
#  $1 : original string with directories
#  $2 : Dictionary variable identifier for destination directories
# Returns
#  0 : valid and declared as associative array
#  1 : not valid name or not declared as associative array
function StripAndOrder() {
   if [[ -z $1 ]]; then
      echo -e "${fmtErr}Empty destination directories given as parameter #1 in '${FUNCNAME[0]}($*)', called from '${FUNCNAME[1]}()'. Aborting.${fmtReset}" >&2
      return 1
   fi
   if CheckAssocArrayParam "$2"; then
      local -n retDict=$2
   else
      echo -e "${fmtErr}Not valid Description dictionary given as parameter #2 in '${FUNCNAME[0]}($*)', called from '${FUNCNAME[1]}()'. Aborting.${fmtReset}" >&2
      exit $err_InternalError
   fi
   local key
   local -a linesArray
   # character ':' is the separator between destinations
   if sort <(echo "${1//:/\n}") | cat --squeeze-blank - | mapfile -t linesArray; then
      for (( i = 0; i < ${#linesArray[@]}; i++ )); do
         local line=${linesArray[$i]}
         line=${line//,/ }
         local isFirst=true
         for item in $line; do
            if $isFirst; then
               key=$item
               isFirst=false
            else
               retDict[$key]=$item
            fi
         done
      done
   else
      return 1
   fi
   return 0
}

#Create '.gitignore' file on base dir of distribution
function CreateGitIgnore() {
   IFS=$'/'
   local dest
   local isFirst=true
   for name in $destPath; do
      if $isFirst; then
         dest="$basePath/$name/.gitignore"
         if [[ -s $dest ]]; then
            break
         fi
         isFirst=false
      else
         IFS=$OLDIFS
         cat >"${dest}" <<EOF
# Ignore everything in this directory
*
# Except this file and subdirectory
!.gitignore
${name}/**
EOF
         echo -e "${fmtOk}Written '$dest'.${fmtReset}"
         break
      fi
   done
   IFS=$OLDIFS
}

#Main
#  $@ : all the parameters of the program
# Exit error level
#   err_InternalError
function Main() {
   ProcessParams "$@"
   echo -e "${fmtInv} ${progName} - ${programDescription} ${fmtReset}"
   local grpNum=0
   #create destination dirs
   if ! mkdir --parents --verbose "$basePath/$destPath"; then
      echo -e "${fmtErr}Cannot create destination path: '$basePath/$destPath'.${fmtReset}" >&2
      exit $err_InternalError
   fi
   CreateGitIgnore #on base dir of distribution
   #loop for deploy groups
   for grp in "${deployGroups[@]}"; do
      echo -e "${fmtBold}Group $((grpNum + 1)) - $grp : ${fmtReset}"
      if [[ -z ${filesFrom[$grp]} ]]; then
         echo -e "${fmtErr}Empty original file list for group '$grp'. Aborting.${fmtReset}" >&2
         exit $err_InternalError
      fi
      local -a sourceFiles
      local noExist="" #list of inexistent files at origin: wouldn't copy them!
      local -i fileIdx=0
      IFS=$':\t\n'
      #determine files to copy, expand to full path and storing in sourceFiles array
      #use ':' character as file separator
      for file in ${filesFrom[$grp]}; do
         if [[ -e $basePath/$file ]]; then
            sourceFiles[$fileIdx]=$basePath/$file
         else
            echo -e "${fmtWarn}Original file '$basePath/$file' not found.${fmtReset}" >&2
            noExist+=" $((fileIdx + 1)) " # add to non existant original file list
         fi
         fileIdx=$((fileIdx + 1))
      done
      IFS=$OLDIFS #restore usual separators
      local -a destDirs
      declare -g -A destDict
      if [[ -z ${filesDest[$grp]} ]]; then
         echo -e "${fmtErr}Empty destination directories list for group '$grp'. Aborting.${fmtReset}" >&2
         exit $err_InternalError
      fi
      #decode destination directories from the group declaration, creating a dictionary
      if ! StripAndOrder ${filesDest[$grp]} destDict; then
         echo -e "${fmtErr}Cannot process destination directories '${filesDest[$grp]}' for group '$grp'. Aborting.${fmtReset}" >&2
         exit $err_InternalError
      fi
      #assing destination directories to each file to copy
      for (( destIdx = 0; destIdx < fileIdx; destIdx++ )); do
         local pattIdx=" $((destIdx + 1)) "
         #test if current index exist on keys of dictionary
         if [[ " ${!destDict[*]} " =~ $pattIdx ]]; then
            destDirs[$destIdx]="${destDict[$destIdx]}"
         else
            #if not, assign definition for 'all'
            destDirs[$destIdx]="${destDict['all']}"
         fi
      done
      #final loop to copy the files
      for (( i = 0; i < fileIdx; i++ )); do
         local pattIdx=" $((i + 1)) "
         #check against non existant file list, to skip the file
         if [[ ! $noExist =~ $pattIdx ]]; then
            if ! mkdir --parents --verbose "$basePath/${destDirs[$i]}"; then
               echo -e "${fmtErr}Cannot create destination path: '$basePath/${destDirs[$i]}'.${fmtReset}" >&2
               exit $err_InternalError
            fi
            #copy the file to destination, keeping original date, creating file if it doesn't exists or updating with a newer version
            cp --verbose --archive --update --target-directory "$basePath/${destDirs[$i]}" "${sourceFiles[$i]}"
         fi
      done
      grpNum=$((grpNum + 1))
   done
   #echo -e "${fmtBold}Creating ZIP file. ${fmtReset}"
}

#Main invocation
Main "$@"
