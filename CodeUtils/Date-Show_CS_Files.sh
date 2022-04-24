#!/usr/bin/env bash
#Show Date value in copyright area on headers of .cs files

set -o pipefail

#===============================================
#global configuration

#file pattern to update
declare fileExt=".cs"
#directories to search on
declare targetDirs="Gear Gear/Disassembler Gear/EmulationCore Gear/GUI \
   Gear/GUI/LogicProbe Gear/PluginSupport Gear/Propeller Gear/Utils"

#===============================================
#Global variables

#Version of this program
declare -r version='1.0'
#Program name without path
declare -g progName=${0##*/}
#Program description
declare -r programDescription="Show copyright dates on headers of \"$fileExt\" files for Gear project."

declare basePathThisProgram=$(
   cd -- "$(dirname "$0")" >/dev/null 2>&1 || exit
   pwd -P
)
#Base path for tree structure
basePath=${basePathThisProgram%/*}

#===============================================
#Set global error level tags
declare -r -i err_OptionNotRecognized=1
declare -r -i err_MissingValue=2
declare -r -i err_InternalError=3

#Definitions for format and colors
if which tput >/dev/null 2>&1 && [[ $(tput -T"$TERM" colors) -ge 8 ]]; then
   declare -x fmtBold="\e[1m"
   declare -x fmtUnd="\e[4m"
   declare -x fmtInv="\e[7m"
   declare -x fmtDebug="\e[94m"
   declare -x fmtWarn="\e[33m"
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
  $progName [-d DIR ]
  $progName [ -h | -v ]

  DIR : Base directory

Options:
  -d | --base-dir  Specify base directory, different from default: '$basePath'.
  -h | --help      Show usage.
  -v | --version   Show program version.
EOF
}

#Process parameters for main program
# Exit error level
#   err_OptionNotRecognized
#   err_MissingValue
function ProcessParams() {
   #define script options
   local options='hvd:'
   local longopts='help,version,base-dir:'
   ! parsed=$(getopt --options=$options --longoptions=$longopts --name "$progName" -- "$@")
   if [[ ${PIPESTATUS[0]} -ne 0 ]]; then
      ShowUsage
      exit $err_OptionNotRecognized
   fi
   # read getopt’s output this way to handle the quoting right, setting value to $1.
   eval set -- "$parsed"
   while true; do
      case $1 in
      -d | --base-dir)
         basePath=$2
         if [[ -z $basePath || $basePath == -- ]]; then
            echo -e "${fmtErr}Base directory is empty for parameter $1. Aborting.${fmtReset}"
            ShowUsage
            exit $err_MissingValue
         elif [[ ! -d $basePath ]]; then
            echo -e "${fmtErr}Base directory not reached: '$2'. Aborting.${fmtReset}"
            ShowUsage
            exit $err_MissingValue
         else
            basePath=${basePath%/*}
            shift 2
         fi
         ;;
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
   #no parameters expected
   if [[ $# -gt 0 ]]; then
      echo -e "${fmtWarn}Extra parameter(s) '$*' ignored.${fmtReset}" >&2
   fi
}

#Main
#  $@ : all the parameters of the program
function Main() {
   ProcessParams "$@"
   echo -e "${fmtInv} ${progName} - ${programDescription} ${fmtReset}"
   #run each file in a parallel process
   parallel --line-buffer find "$basePath/{}" -maxdepth 1 -type f -name "*$fileExt" -print ::: $targetDirs |
      parallel --line-buffer --tagstring '{/}' sed -E -n -e \'s/^[\* ]+[Cc]opyright \([[:digit:],-]+\).*$/\\1/p\; T \; Q\' {} |
      stdbuf --output=L sort | stdbuf --output=L cat --number-nonblank |
      stdbuf --output=L cat <(echo -e "${fmtBold}${fmtUnd}Number\tFile\tDate\t${fmtReset}") - |
      stdbuf --output=L column -s '	' -t
}

#Main invocation
Main "$@"
