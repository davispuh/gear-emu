#!/usr/bin/env bash
#Manage headers of .cs files

set -o pipefail

#===============================================
#global configuration

#file pattern to update
declare fileExt=".cs"
#directories to search on
declare targetDirs="Gear Gear/Disassembler Gear/EmulationCore Gear/GUI \
   Gear/GUI/LogicProbe Gear/PluginSupport Gear/Propeller Gear/Utils"

#===============================================
#global variables
declare -r version='1'
declare -g progName=${0##*/}

declare -g -x mode
declare -g -x object
declare -g -x newValue
declare -g -x retVal

declare basePath=$(
   cd -- "$(dirname "$0")" >/dev/null 2>&1 || exit
   pwd -P
)
#remove current directory from path
basePath=${basePath%/*}

#===============================================
#Set global error level tags
declare -r -i err_OptionNotRecognized=1
declare -r -i err_InternalError=2

#Definitions for format and colors
if which tput >/dev/null 2>&1 && [[ $(tput -T"$TERM" colors) -ge 8 ]]; then
   declare -x fmtBold="\e[1m"
   declare -x ftmUnd="\e[4m"
   declare -x ftmInv="\e[7m"
   declare -x fmtDebug="\e[94m"
   declare -x fmtUser="\e[33m"
   declare -x fmtErr="\e[31m"
   declare -x fmtOk="\e[32m"
   declare -x fmtReset="\e[0m"
   declare -x fmtSection="\e[1;44;33m"
fi

declare -A objectsDescriptions
objectsDescriptions['dates']="Copyright dates (as \"1996\" or \"1996-2015\")"

#===============================================
#Functions

#Show usage
function ShowUsage() {
   local objsDetails=""
   local obj
   #generate the object details
   for obj in "${!objectsDescriptions[@]}"; do
      objsDetails+=$(echo -e "  $obj\t:\t${objectsDescriptions[$obj]}.\n\r")
   done
   objsDetails=$(echo "$objsDetails" | column -s '	' -t | sort)
   #generate the output
   cat <<EOF >&2
Usage:
  $progName -s OBJ
  $progName -u OBJ "NEWVALUE"
  $progName [ -h | -v ]

OBJ:
${objsDetails}

Options:
  -s | --show      Show object.
  -u | --update    Update object.
  -h | --help      Show usage.
  -v | --version   Show program version.
EOF
}

#Process parameters for main installer
# Exit code
#   err_OptionNotRecognized
function ProcessParams() {
   #define script options
   local options='-s:u:hv'
   local longopts='show:,update:,help,version'
   ! parsed=$(getopt --options=$options --longoptions=$longopts --name "$progName" -- "$@")
   if [[ ${PIPESTATUS[0]} -ne 0 ]]; then
      ShowUsage
      exit $err_OptionNotRecognized
   fi
   # read getopt’s output this way to handle the quoting right, setting value to $1.
   eval set -- "$parsed"
   while true; do
      case $1 in
      -s | --show)
         mode=show
         shift
         if ! SetupByObject $mode $*; then
            ShowUsage
            exit $err_OptionNotRecognized
         elif [[ $retVal -gt 0 ]]; then
            shift $((retVal))
         fi
         ;;
      -u | --update)
         mode=update
         shift
         if ! SetupByObject $mode $*; then
            ShowUsage
            exit $err_OptionNotRecognized
         elif [[ $retVal -gt 0 ]]; then
            shift $((retVal))
         fi
         ;;
      -h | --help)
         echo "Description: Mass management of .cs files"
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
}

#
function SetupByObject() {
   if [[ -z $1 ]]; then
      echo -e "${fmtErr}Empty mode given as parameter #1 in '${FUNCNAME[0]}($*)', called from '${FUNCNAME[1]}()'. Aborting.${fmtReset}" >&2
      return $err_InternalError
   fi
   local mode=$1
   if [[ -z $2 ]]; then
      echo -e "${fmtErr}Empty object name given as parameter #2 in '${FUNCNAME[0]}($*)', called from '${FUNCNAME[1]}()'. Aborting.${fmtReset}" >&2
      return $err_InternalError
   fi
   local -l obj=$2
   retVal=0
   case $obj in
   dates)
      case $mode in
      show)
         object=$obj
         retVal=1
         ;;
      update)
         object=$obj
         if [[ -z $3 || $3 == -- ]]; then
            echo -e "${fmtErr}Empty Date value given as parameter #3 in '${FUNCNAME[0]}($*)', called from '${FUNCNAME[1]}()'. Aborting.${fmtReset}" >&2
            return 1
         elif [[ $3 =~ [[:digit:]-]+ ]]; then
            newValue=$3
            retVal=2
         else
            echo -e "${fmtErr}Value '$3' given as parameter #3 is not a valid Date in '${FUNCNAME[0]}($*)', called from '${FUNCNAME[1]}()'. Aborting.${fmtReset}" >&2
            return 1
         fi
         ;;
      *) : ;;
      esac
      ;;
   *)
      echo -e "${fmtErr}Object not recognized: '$1'.${fmtReset}" >&2
      ShowUsage
      exit $err_OptionNotRecognized
      ;;
   esac
   return 0
}

#
function Main() {
   #to replace
   #YEAR="2022"
   #parallel --tagstring '{/} :' sed -E -n -e \'s/^\([\* ]+[Cc]opyright \)\([[:digit:]]+\)\(.*\)$/\\1$YEAR\\3/p\; T \; Q\' {} ::: $FILES

   #run each file in a parallel process
   #parallel --bar --tagstring '{#}\t{/}' sed -E -n -e \'s/^[\* ]+[Cc]opyright \([[:digit:]]+\).*$/\\1/\; T \; Q\' {} ::: ${FILES[*]}
   parallel --line-buffer find "$basePath/{}" -maxdepth 1 -type f -name "*$fileExt" -print ::: $targetDirs |
      parallel --line-buffer --tagstring '{/}' sed -E -n -e \'s/^[\* ]+[Cc]opyright \([[:digit:]-]+\).*$/\\1/p\; T \; Q\' {} |
      sort | column -t
}

ProcessParams $*
